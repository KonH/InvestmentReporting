<template>
	<div class="card" style="width: 20rem">
		<div class="card-body">
			<h4>{{ tag }}</h4>
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { BrokerDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';
import { AssetTagSetDto } from '@/api/meta';

@Options({
	name: 'TagCard',
})
export default class TagCard extends Vue {
	@Prop()
	tag!: string;
}
</script>
