<template>
	<!-- ISIN -->
	<th scope="row">{{ asset.isin }}</th>
	<!-- Name -->
	<td>{{ asset.name }}</td>
	<!-- Tags -->
	<td>
		<div class="row pl-3">
			<div v-for="tag in asset.tags" :key="tag">
				<tag-card :tag="tag" class="mr-2" />
			</div>
			<add-tag-card />
		</div>
	</td>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import { AssetTagSetDto } from '@/api/meta';
import TagCard from '@/component/tagCard.vue';
import AddTagCard from '@/component/addTagCard.vue';

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
