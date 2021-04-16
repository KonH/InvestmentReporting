<template>
	<div class="card" style="width: 8rem">
		<div class="card-body">
			<router-link to="/broker/new" class="btn btn-outline-primary btn-block">+</router-link>
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import { AssetTagSetDto } from '@/api/meta';

@Options({
	name: 'AddTagCard',
})
export default class AddTagCard extends Vue {
	@Prop()
	asset!: AssetTagSetDto;
}
</script>
