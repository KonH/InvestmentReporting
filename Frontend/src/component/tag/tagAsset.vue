<template>
	<!-- ISIN -->
	<th scope="row">{{ asset.isin }}</th>
	<!-- Name -->
	<td>{{ asset.name }}</td>
	<!-- Tags -->
	<td>
		<span v-for="tag in asset.tags" :key="tag">
			<tag-card :asset="asset" :tag="tag" class="mr-2" />
		</span>
		<add-tag-card :asset="asset" />
	</td>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import { AssetTagSetDto } from '@/api/meta';
import TagCard from '@/component/tag/tagCard.vue';
import AddTagCard from '@/component/tag/addTagCard.vue';

@Options({
	name: 'TagAsset',
	components: {
		TagCard,
		AddTagCard,
	},
})
export default class TagAsset extends Vue {
	@Prop()
	asset!: AssetTagSetDto;
}
</script>
